

# Janono.WindowsAzure.Storage.Table

[![Build Status](https://dev.azure.com/janono-pub/Janono.WindowsAzure.Storage.Table/_apis/build/status/janusznowak.Janono.WindowsAzure.Storage.Table?branchName=master)](https://dev.azure.com/janono-pub/Janono.WindowsAzure.Storage.Table/_build/latest?definitionId=20&branchName=master)

## Janono.WindowsAzure.Storage.Table

 Is still under active development at https://dev.azure.com/janono-pub/Janono.WindowsAzure.Storage.Table.

## Janono.WindowsAzure.Storage.Table?

Janono.Azure.DocumentsDB.Scale is implemenation of Crud abstract operations library for Azure Storage Table. By providing contract class is simplify operations. Library is using performance optimized way for bulk operations.

Package name                              | Stable                 
------------------------------------------|-------------------------------------------
`Janono.WindowsAzure.Storage.Table`          | [![NuGet](https://img.shields.io/nuget/v/Janono.WindowsAzure.Storage.Table.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Janono.WindowsAzure.Storage.Table/) 



## Example of usage Janono.WindowsAzure.Storage.Table

```
   internal class TestContract : AzureTableEntity
        {
        }

        [Fact]
        public void ExampleOfUseBulkOptimizeInsertCreate ()
        {
            string storageAccount = "janonostorage";
            string storageKey = System.Environment.GetEnvironmentVariable("storageKey");

            string tableName = "testTableNetF";       
            var stor = new AzureTableStorage<TestContract>(
            new AzureTableSettings(
               storageAccount: storageAccount,
               storageKey: storageKey,
               tableName: tableName));

            List<TestContract> listtest = new List<TestContract>();
            for (int i = 0; i < 20000; i++)
            {
                var t = new TestContract();
                t.PartitionKey = "_";
                t.RowKey = i.ToString();
                listtest.Add(t);
            }
            stor.GetTableAsyncCreateIfNotExistsAsync().GetAwaiter().GetResult();
            stor.InsertReplace(listtest).GetAwaiter().GetResult();
            stor.DeleteIfExistsAsync();
        }
        ```
