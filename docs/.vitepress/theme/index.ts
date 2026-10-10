import DefaultTheme from 'vitepress/theme'
import type { Theme } from 'vitepress'
import DownloadButtons from './DownloadButtons.vue'
import './custom.css'

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    app.component('DownloadButtons', DownloadButtons)
  },
} satisfies Theme
