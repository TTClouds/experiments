open System
open System.Threading.Tasks
open Grpc.Core
open CsProto
open Grpc.Core
open CsProto
open System.Diagnostics
open System.Threading

let test1 (client: Greeter.GreeterClient) times =
    for i in 1 .. times do
        let request = HelloRequest()
        request.Name <- sprintf "Iskander %d" i
        let reply = client.SayHello(request)
        // printfn "Responded: %s" reply.Message
        do()

let test2 (client: Greeter.GreeterClient) times =
    let call = client.SayHelloAgain()
    let rec loop i = async {
        if i > times then
            return ()
        else
            let request = HelloRequest()
            request.Name <- sprintf "Iskander %d" i
            do! call.RequestStream.WriteAsync(request) |> Async.AwaitTask
            let! moved = call.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
            if not moved then
                return ()
            else
                do call.ResponseStream.Current.Message |> ignore
                return! loop (i + 1)
    }
    for i in 1 .. times do
        let request = HelloRequest()
        request.Name <- sprintf "Iskander %d" i
        let reply = client.SayHello(request)
        // printfn "Responded: %s" reply.Message
        do()
    Async.RunSynchronously (loop 1)

[<EntryPoint>]
let main argv =
    let channel = Channel("127.0.0.1:50051", ChannelCredentials.Insecure)
    do channel.ConnectAsync().Wait()
    let client = Greeter.GreeterClient(channel)
    
    let times = 10000
    let watch = Stopwatch.StartNew()
    test2 client times
    do watch.Stop()

    printfn "Elapsed %f ms" watch.Elapsed.TotalMilliseconds
    let perOp = watch.Elapsed.TotalMilliseconds / float(times)
    let opsPerSec = 1000.0 / perOp
    printfn "Per operation %f ms" perOp
    printfn "%f operations per second" opsPerSec

    do channel.ShutdownAsync().Wait()
    do printfn "Press any key to stop the client ..."
    do Console.ReadKey() |> ignore
    
    0 // return an integer exit code
