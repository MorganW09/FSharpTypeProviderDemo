open FSharp.Data

type Product =
    {
        ProductID : int
        Name : string
        Color : string option
    }
[<Literal>]
let connectionString = 
    @"Server=localhost;Database=AdventureWorksLT2017;Trusted_Connection=True;"

let getProducts () = 
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [SalesLT].[Product]" , connectionString>(connectionString)
    
    cmd.Execute()

let getRedProducts () =
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [SalesLT].[Product]
        WHERE Color = @color", connectionString>(connectionString)

    cmd.Execute("Red");

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let records = getProducts()
    records |> printfn "%A"
    let product722 = records |> Seq.filter (fun x -> x.ProductID = 722)
    printfn "record 722 %A" product722
    
    let redRecords = getRedProducts()
    redRecords |> printfn "%A"
    Seq.length redRecords |> printfn "number of red records %d" 
    0
