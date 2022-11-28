import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default ({mode}) => {
  process.env = {...process.env, ...loadEnv(mode, process.cwd())};

  const apiUrl = process.env.VITE_API_URL;
  const wsUrl = process.env.VITE_WS_URL;
  
  return defineConfig({
    plugins: [vue()],
    server: {
      proxy: {
        '/api/games': {
          target: apiUrl,
          changeOrigin: true,
        },
        '/ws': {
          target: wsUrl,
          changeOrigin: true,
          ws: true
        }
      }
    }
  });
}