module.exports = {
    productionSourceMap: process.env.NODE_ENV != 'production',
    chainWebpack: (config) => {
        // Remove prefetch plugin and that's it!
        config.plugins.delete('prefetch')
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