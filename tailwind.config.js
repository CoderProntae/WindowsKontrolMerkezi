/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['JetBrains Mono', 'monospace'],
      },
      colors: {
        surface: '#0f0f12',
        card: '#16161a',
        border: '#2a2a2e',
        accent: '#7c3aed',
        accentDim: '#5b21b6',
      }
    },
  },
  plugins: [],
}
