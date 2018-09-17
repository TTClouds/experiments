const Koa = require("koa");
const {} = require("rxjs");
const MongoClient = require("mongodb").MongoClient;
const app = new Koa();

const conf = {
  SERVER_PORT: parseInt(process.env.SERVER_PORT || "3030", 10),
  SERVER_HOST: process.env.SERVER_PORT || undefined
};

const propertyManager = (function() {})();

app.use(async ctx => {
  ctx.body = "Hello world";
});

const server = app.listen(conf.SERVER_PORT, conf.SERVER_HOST, () =>
  console.log(`SERVER listenning on ${conf.SERVER_HOST}:${conf.SERVER_PORT}`)
);
server.on("close", () => console.log("SERVER close"));
server.on("error", err => console.log("SERVER error: ", err));
server.on("connection", socket => {
  console.log("SERVER socket connection");
  // console.log(JSON.stringify({
  //   bufferSize: socket.bufferSize,
  //   bytesRead: socket.bytesRead,
  //   bytesWritten: socket.bytesWritten,
  //   connecting: socket.connecting,
  //   destroyed: socket.destroyed,
  //   localAddress: socket.localAddress,
  //   localPort: socket.localPort,
  //   readableLength: socket.readableLength,
  //   writableLength: socket.writableLength,
  // }));
});
