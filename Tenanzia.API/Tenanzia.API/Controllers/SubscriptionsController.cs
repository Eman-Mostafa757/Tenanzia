using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Tenanzia.API.DTOs.Subscriptions;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;
using Subscription = Tenanzia.API.Models.Subscription;
namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly IConfiguration _config;
        private readonly SubscriptionLimitService _subscriptionLimitService;
        public SubscriptionsController(TenanziaContext context, ITenantService tenantService, IConfiguration config, SubscriptionLimitService subscriptionLimitService)
        {
            _context = context;
            _tenantService = tenantService;
            _config = config;
            _subscriptionLimitService = subscriptionLimitService;
        }

        // GET: api/subscriptions/current
        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            var tenantId = _tenantService.GetTenantId();

            var subscription = _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId && s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id,
                    PlanName = s.Plan.Name,
                    Price = s.Plan.Price,
                    Status = s.Status,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                }).FirstOrDefault();

            if (subscription == null)
                return NotFound("No active subscription found");

            return Ok(subscription);
        }

        // GET: api/subscriptions/plans
        [AllowAnonymous]
        [HttpGet("plans")]
        public IActionResult GetPlans()
        {
            var plans = _context.Plans
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.MaxCustomers,
                    p.MaxTasks
                }).ToList();

            return Ok(plans);
        }

        // POST: api/subscriptions/checkout
        [HttpPost("checkout")]
        public IActionResult CreateCheckout(CreateCheckoutDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            var plan = _context.Plans
                .FirstOrDefault(p => p.Name == dto.PlanName && p.StripePriceId != null);

            if (plan == null)
                return BadRequest("Invalid plan or plan is free");

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Price = plan.StripePriceId,
                    Quantity = 1
                }
            },
                SuccessUrl = "https://localhost:44302/api/Subscriptions/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:44302/api/Subscriptions/cancel",
                Metadata = new Dictionary<string, string>
            {
                { "TenantId", tenantId.ToString() },
                { "PlanId", plan.Id.ToString() }
            }
            };

            var service = new Stripe.Checkout.SessionService();
            var session = service.Create(options);

            return Ok(new { checkoutUrl = session.Url });
        }

        // GET: api/subscriptions/success
        [AllowAnonymous]
        [HttpGet("success")]
        public IActionResult Success([FromQuery] string session_id)
        {
            var service = new Stripe.Checkout.SessionService();
            var session = service.Get(session_id);

            if (session.PaymentStatus != "paid" && session.Status != "complete")
                return BadRequest("Payment not completed");

            var tenantId = int.Parse(session.Metadata["TenantId"]);
            var planId = int.Parse(session.Metadata["PlanId"]);

            // إلغاء الـ subscription القديمة
            var oldSubs = _context.Subscriptions
                .Where(s => s.TenantId == tenantId && s.Status == "Active")
                .ToList();

            foreach (var old in oldSubs)
            {
                old.Status = "Cancelled";
                old.EndDate = DateTime.UtcNow;
            }

            // إنشاء subscription جديدة
            _context.Subscriptions.Add(new Subscription
            {
                TenantId = tenantId,
                PlanId = planId,
                Status = "Active",
                StartDate = DateTime.UtcNow,
                StripeSubscriptionId = session.SubscriptionId
            });

            _context.SaveChanges();

            return Ok("Subscription activated successfully!");
        }

        // GET: api/subscriptions/cancel
        [AllowAnonymous]
        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok("Payment cancelled");
        }

        // POST: api/subscriptions/webhook
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _config["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                if (stripeEvent.Type == "customer.subscription.deleted")
                {
                    var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                    if (stripeSubscription != null)
                    {
                        var sub = _context.Subscriptions
                            .FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscription.Id);

                        if (sub != null)
                        {
                            sub.Status = "Cancelled";
                            sub.EndDate = DateTime.UtcNow;

                            // رجّعه للـ Free Plan
                            var freePlan = _context.Plans.FirstOrDefault(p => p.Name == "Free");
                            if (freePlan != null)
                            {
                                _context.Subscriptions.Add(new Subscription
                                {
                                    TenantId = sub.TenantId,
                                    PlanId = freePlan.Id,
                                    Status = "Active",
                                    StartDate = DateTime.UtcNow
                                });
                            }

                            _context.SaveChanges();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        [HttpGet("limits")]
        public IActionResult GetLimits([FromServices] SubscriptionLimitService subscriptionService)
        {
            var tenantId = _tenantService.GetTenantId();
            var limits = _subscriptionLimitService.GetLimits(tenantId);
            return Ok(limits);
        }

        [HttpPost("downgrade")]
        public IActionResult Downgrade()
        {
            var tenantId = _tenantService.GetTenantId();

            // إلغاء الـ subscription الحالية
            var current = _context.Subscriptions
                .FirstOrDefault(s => s.TenantId == tenantId && s.Status == "Active");

            if (current != null)
            {
                current.Status = "Cancelled";
                current.EndDate = DateTime.UtcNow;
            }

            // رجوع للـ Free
            var freePlan = _context.Plans.FirstOrDefault(p => p.Name == "Free");
            if (freePlan != null)
            {
                _context.Subscriptions.Add(new Tenanzia.API.Models.Subscription
                {
                    TenantId = tenantId,
                    PlanId = freePlan.Id,
                    Status = "Active",
                    StartDate = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
            return Ok("Downgraded to Free Plan");
        }
    }
}
