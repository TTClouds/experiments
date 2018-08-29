const Koa = require('koa');
const MongoClient = require('mongodb').MongoClient;
const app = new Koa();

const propertyManager = (function() {

})();

app.use(async ctx => {
  ctx.body = 'Hello world';
});

app.listen(3030);