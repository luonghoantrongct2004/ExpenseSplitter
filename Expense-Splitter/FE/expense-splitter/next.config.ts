import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
};
// next.config.js
module.exports = {
  experimental: {
    turbo: {
      // Tắt UI indicator
      ui: false
    }
  }
}

export default nextConfig;
