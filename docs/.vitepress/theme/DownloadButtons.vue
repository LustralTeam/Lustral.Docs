<script setup lang="ts">
// Downloads the latest Lustral release directly. Each release's zips have fixed names, and GitHub serves the latest
// release's assets under releases/latest/download, so these links do not change between releases.
const latest = 'https://github.com/LustralTeam/Lustral/releases/latest/download/'
</script>

<template>
  <div class="download-buttons">
    <a class="download-button brand" :href="latest + 'lustral-win-x64-il2cpp.zip'">
      <span class="download-title">Download for IL2CPP games</span>
      <span class="download-detail">The game folder contains <code>GameAssembly.dll</code></span>
    </a>
    <a class="download-button alt" :href="latest + 'lustral-win-x64-mono.zip'">
      <span class="download-title">Download for Mono games</span>
      <span class="download-detail">All other games</span>
    </a>
  </div>
</template>

<style scoped>
.download-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin: 16px 0;
}

.download-button {
  display: flex;
  flex: 1 1 240px;
  flex-direction: column;
  gap: 2px;
  border: 1px solid transparent;
  border-radius: 12px;
  padding: 12px 20px;
  text-decoration: none;
  transition: color 0.25s, border-color 0.25s, background-color 0.25s;
}

.download-button.brand {
  border-color: var(--vp-button-brand-border);
  color: var(--vp-button-brand-text);
  background-color: var(--vp-button-brand-bg);
}

.download-button.brand:hover {
  border-color: var(--vp-button-brand-hover-border);
  color: var(--vp-button-brand-hover-text);
  background-color: var(--vp-button-brand-hover-bg);
}

.download-button.alt {
  border-color: var(--vp-button-alt-border);
  color: var(--vp-button-alt-text);
  background-color: var(--vp-button-alt-bg);
}

.download-button.alt:hover {
  border-color: var(--vp-button-alt-hover-border);
  color: var(--vp-button-alt-hover-text);
  background-color: var(--vp-button-alt-hover-bg);
}

.download-title {
  font-size: 16px;
  font-weight: 600;
  line-height: 24px;
}

.download-detail {
  font-size: 13px;
  line-height: 20px;
  opacity: 0.85;
}

.download-detail code {
  border-radius: 4px;
  padding: 1px 5px;
  color: inherit;
  font-size: 12px;
  background-color: rgba(255, 255, 255, 0.18);
}

.download-button.alt .download-detail code {
  background-color: var(--vp-c-default-soft);
}
</style>
