open System
open System.Threading.Tasks
open Grpc.Core
open CsProto
open System.Threading

type GreeterImpl() =
    inherit Greeter.GreeterBase()

    override __.SayHello(request, context) = 
        let message = sprintf "Hello %s back!" request.Name
        let response = HelloResponse()
        do response.Message <- message
        // printfn "Responding with: %s" message
        Task.FromResult response

    override __.SayHelloAgain(requestStream, responseStream, context) =
        let rec loop () = async {
            let! moved = requestStream.MoveNext(context.CancellationToken) |> Async.AwaitTask
            if moved then
                let message = sprintf "Hello %s you!" requestStream.Current.Name
                let response = HelloResponse()
                do response.Message <- message
                do! responseStream.WriteAsync(response) |> Async.AwaitTask
                return! loop()
            else
                return ()
        }
        let result = Async.StartAsTask(loop())
        result :> Task

[<EntryPoint>]
let main argv =
    let port = 50051
    let server = Server()
    do GreeterImpl() |> Greeter.BindService |> server.Services.Add
    do ServerPort("localhost", port, ServerCredentials.Insecure) |> server.Ports.Add |> ignore
    do server.Start()

    do printfn "Greeter server listenning on port %d" port
    do printfn "Press any key to stop the server ..."
    do Console.ReadKey() |> ignore

    do server.ShutdownAsync().Wait()

    0 // return an integer exit code
