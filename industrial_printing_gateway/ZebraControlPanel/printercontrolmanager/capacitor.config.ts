import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'ionic.printercontrolmanager',
  appName: 'printercontrolmanager',
  webDir: 'dist',
  android: {
    allowMixedContent: true, // This allows loading insecure (HTTP) content
  },
  // Optional: for local development/testing with cleartext traffic
  server: {
    androidScheme: 'http',
    cleartext: true, // This enables all HTTP (cleartext) requests on Android
  },
};

export default config;
