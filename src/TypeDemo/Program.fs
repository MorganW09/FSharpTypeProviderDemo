open FSharp.Data
open System
open System.Data.SqlClient

//[<Literal>]
//let connectionString = 
//    "Server=localhost;Database=AdventureWorksLT2017;Trusted_Connection=True;"

[<Literal>]
let connectionString = 
    "Server=localhost;Database=AdventureWorks2017;Trusted_Connection=True;"
    

type AdventureWorks2017 = SqlProgrammabilityProvider<connectionString>


type Product =
 {
     Name : string
     ProductNumber : string
     MakeFlag : Boolean
     FinishedGoodsFlag : Boolean
     Color : string
     SafetyStockLevel : int16
     ReorderPoint : int16
     StandardCost : decimal
     ListPrice : decimal
     DaysToManufacture : int
     SellStartDate : DateTime
     Rowguid : Guid
     ModifiedDate : DateTime
 }

 
type NewDepartment = 
 {
     Name : string
     GroupName : string
     ModifiedDate : DateTime option
 }

let getProducts () = 
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [Production].[Product]" , connectionString>(connectionString)
    
    cmd.Execute()

let getRedProducts (color) =
    let cmd = new SqlCommandProvider<"
        SELECT TOP (1000) 
        [ProductID]
        ,[Name]
        ,[Color]
        FROM [Production].[Product]
        WHERE Color = @color", connectionString>(connectionString)

    cmd.Execute(color)

 
let insertProduct (product : Product) =
    let cmd = new SqlCommandProvider<"
    INSERT INTO [Production].[Product] 
        (Name, ProductNumber, MakeFlag, FinishedGoodsFlag, Color, SafetyStockLevel, ReorderPoint, StandardCost, ListPrice, DaysToManufacture, SellStartDate, rowguid, ModifiedDate)
    VALUES (@Name, @ProductNumber, @MakeFlag, @FinishedGoodsFlag, @Color, @SafetyStockLevel, @ReorderPoint, @StandardCost, @ListPrice, @DaysToManufacture, @SellStartDate, @rowguid, @ModifiedDate)", connectionString>(connectionString)

    cmd.Execute(product.Name, product.ProductNumber, product.MakeFlag, product.FinishedGoodsFlag, product.Color, 
        product.SafetyStockLevel, product.ReorderPoint, product.StandardCost, product.ListPrice, product.DaysToManufacture,
        product.SellStartDate, product.Rowguid, product.ModifiedDate)

let deleteProduct (productNumber) =
    let cmd = new SqlCommandProvider<"
        DELETE FROM [Production].[Product]
        WHERE ProductNumber = @ProductNumber", connectionString>(connectionString)

    cmd.Execute(productNumber)

//Make sure you return a list or array
//https://github.com/fsprojects/FSharp.Data.SqlClient/issues/321
let getEmployeeManagers (businesEntityID : int) =
    use cmd = new AdventureWorks2017.dbo.uspGetEmployeeManagers(connectionString)
    cmd.Execute(businesEntityID) |> Seq.toList

let insertDepartment(newDepartment) =
    use table = new AdventureWorks2017.HumanResources.Tables.Department()
    use conn = new SqlConnection(connectionString)
    conn.Open()
    table.AddRow(newDepartment.Name, newDepartment.GroupName, newDepartment.ModifiedDate)
    table.Update(conn)



type PhoneNumberType = 
    SqlEnumProvider<"SELECT Name, PhoneNumberTypeID FROM Person.PhoneNumberType ORDER BY PhoneNumberTypeID", connectionString>

let selectAllCellPhones() =
    let cmd = new SqlCommandProvider<"SELECT * FROM Person.PersonPhone 
        WHERE PhoneNumberTypeID = @phoneNumberType", connectionString>(connectionString)

    cmd.Execute(PhoneNumberType.Cell)

[<EntryPoint>]
let main argv =
    //basic select
    let records = getProducts()
    records |> printfn "%A"
    
    //select with parameter
    let redRecords = getRedProducts("Red")
    redRecords |> printfn "%A"
    Seq.length redRecords |> printfn "number of red records %d"
    
    //insert
    let newProduct = { Name = "Touring Double Rear Wheel"; ProductNumber = "RW-T905a"; MakeFlag = true; 
        FinishedGoodsFlag = true; Color = "Red"; SafetyStockLevel = 1s; ReorderPoint = 5s; StandardCost = 200.82m; 
        ListPrice = 400.1m; DaysToManufacture = 20; SellStartDate = DateTime.UtcNow;  Rowguid = Guid.NewGuid();
        ModifiedDate = DateTime.UtcNow }
    let insertedCount = insertProduct(newProduct)
    printfn "Products inserted: %i" insertedCount

    //delete
    let deletedCount = deleteProduct("RW-T905a")
    printfn "Products deleted: %i" deletedCount

    //get via SqlProgrammabilityProvider
    let employeeManagers = getEmployeeManagers(120)
    printfn "Employee Managers: %A" employeeManagers

    //insert via SqlProgrammabilityProvider
    let newDepartment = { Name = "DepartmentName"; GroupName = "GroupName"; ModifiedDate = Some DateTime.UtcNow }
    let recordsInserted = insertDepartment(newDepartment)
    printfn "Inserted %i department" recordsInserted
    0
