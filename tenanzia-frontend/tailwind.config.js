/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        'tn-bg': 'var(--bg-primary)',
        'tn-surface': 'var(--bg-surface)',
        'tn-secondary': 'var(--bg-secondary)',
        'tn-border': 'var(--border)',
        'tn-text': 'var(--text-primary)',
        'tn-muted': 'var(--text-secondary)',
        'tn-pink': 'var(--pink)',
        'tn-pink-dark': 'var(--pink-dark)',
        'tn-pink-light': 'var(--pink-light)',
        // 'tn-bg': '#0D0D0F',
        // 'tn-surface': '#111114',
        // 'tn-border': '#1E1E24',
        // 'tn-text': '#F0F0F2',
        // 'tn-muted': '#666666',
        // 'tn-pink': '#D4537E',
        // 'tn-purple': '#7F77DD',
        // 'tn-teal': '#5DCAA5',
        // 'tn-amber': '#EF9F27',
      }
    },
  },
  plugins: [],
}