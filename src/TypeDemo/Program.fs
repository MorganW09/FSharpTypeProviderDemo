open FSharp.Data
open System

[<Literal>]
let connectionString = 
    "Server=localhost;Database=AdventureWorksLT2017;Trusted_Connection=True;"

[<Literal>]
let connectionString2 = 
    "Server=localhost;Database=AdventureWorks2017;Trusted_Connection=True;"
    

type AdventureWorks2017 = SqlProgrammabilityProvider<connectionString2>

type Product =
    {
        Name : string
        ProductNumber : string
        Color : string
        StandardCost : decimal
        ListPrice : decimal
        Size : string
        Weight : decimal
        ProductCategoryID : int
        ProductModelID : int
        SellStartDate : DateTime
        SellEndDate : DateTime
        DiscontinuedDate : DateTime
        Rowguid : Guid
        ModifiedDate : DateTime
    }

let getProducts () = 
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [SalesLT].[Product]" , connectionString>(connectionString)
    
    cmd.Execute()

let getRedProducts (color) =
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [SalesLT].[Product]
        WHERE Color = @color", connectionString>(connectionString)

    cmd.Execute(color)

let insertProduct (product : Product) =
    let cmd = new SqlCommandProvider<"
    INSERT INTO [SalesLT].[Product] 
        (Name, ProductNumber, Color, StandardCost, 
        ListPrice, Size, Weight, ProductCategoryID, ProductModelID, SellStartDate, 
        SellEndDate, DiscontinuedDate, rowguid, ModifiedDate)
    VALUES (@Name, @ProductNumber, @Color, @StandardCost, @ListPrice, @Size, 
        @Weight, @ProductCategoryID, @ProductModelID, @SellStartDate, @SellEndDate,
        @DistinuedDate, @rowguid, @ModifiedDate)", connectionString>(connectionString)

    cmd.Execute(product.Name, product.ProductNumber, product.Color, product.StandardCost, product.ListPrice, 
        product.Size, product.Weight, product.ProductCategoryID, product.ProductModelID, product.SellStartDate, 
        product.SellEndDate, product.DiscontinuedDate, product.Rowguid, product.ModifiedDate)

let deleteProduct (productNumber) =
    let cmd = new SqlCommandProvider<"
        DELETE FROM [SalesLT].[Product]
        WHERE ProductNumber = @ProductNumber", connectionString>(connectionString)

    cmd.Execute(productNumber)

//Make sure you return a list or array
//https://github.com/fsprojects/FSharp.Data.SqlClient/issues/321
let getEmployeeManagers (businesEntityID : int) =
    use cmd = new AdventureWorks2017.dbo.uspGetEmployeeManagers(connectionString2)
    cmd.Execute(businesEntityID) |> Seq.toList
[<EntryPoint>]
let main argv =
    let records = getProducts()
    records |> printfn "%A"
    let product722 = records |> Seq.filter (fun x -> x.ProductID = 722)
    printfn "record 722 %A" product722
    
    let redRecords = getRedProducts("Red")
    redRecords |> printfn "%A"
    Seq.length redRecords |> printfn "number of red records %d"
    
    let newProduct = { Name = "Touring Double Rear Wheel"; ProductNumber = "RW-T905a"; Color = "Red"; 
        StandardCost = 200.82m; ListPrice = 400.1m; Size = "40"; Weight = 1200.44m; ProductCategoryID = 21; 
        ProductModelID = 14; SellStartDate = DateTime.UtcNow; SellEndDate = DateTime.MaxValue; 
        DiscontinuedDate = DateTime.MaxValue; Rowguid = Guid.NewGuid(); ModifiedDate = DateTime.UtcNow }
    let insertedCount = insertProduct(newProduct)
    printfn "Products inserted: %i" insertedCount

    let deletedCount = deleteProduct("RW-T905a")
    printfn "Products deleted: %i" deletedCount

    let employeeManagers = getEmployeeManagers(120)
    printfn "Employee Managers: %A" employeeManagers
    0
