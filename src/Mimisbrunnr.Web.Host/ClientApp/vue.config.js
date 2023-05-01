module.exports = {
    productionSourceMap: process.env.NODE_ENV != 'production',
    chainWebpack: (config) => {
        // Remove prefetch plugin and that's it!
        config.plugins.delete('prefetch') 
    }
  };