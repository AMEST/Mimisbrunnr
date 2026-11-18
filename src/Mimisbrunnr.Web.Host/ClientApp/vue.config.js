// Shared by the page and its worker, so caches can be retained for open tabs.
process.env.VUE_APP_BUILD_ID = require('crypto').randomBytes(16).toString('hex')

module.exports = {
    productionSourceMap: process.env.NODE_ENV != 'production',
    chainWebpack: (config) => {
        // Remove prefetch plugin and that's it!
        config.plugins.delete('prefetch')
    },
    pwa: {
        workboxPluginMode: 'InjectManifest',
        workboxOptions: {
            swSrc: './src/service-worker.js',
            // HTML and mutable URLs always use the network. Only immutable
            // assets belong to a release's precache.
            include: [/\.[a-f0-9]{8,}\.(?:js|css)$/i]
        }
    },
    configureWebpack: {
        resolve: {
            fallback: {
                fs: false,
                path: require.resolve('path-browserify')
            }
        },
        performance: false,
        ignoreWarnings: [
            (warning) =>
                warning.message &&
                (/export 'default' \(imported as 'style\d+'\)/.test(warning.message) ||
                    /Replace color-adjust to print-color-adjust/.test(warning.message))
        ]
    }
};
