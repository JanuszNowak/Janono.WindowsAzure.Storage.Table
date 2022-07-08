namespace Janono.WindowsAzure.Storage.Table.XUnitTestNetFramework
{
    using System.Collections.Generic;
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            string storageAccount = "janonostorage";
            string storageKey = System.Environment.GetEnvironmentVariable("storageKey");

            var osNameAndVersion = "";// System.Runtime.InteropServices.RuntimeInformation.OSDescription;

            string tableName = "testTableNetF" + osNameAndVersion;
            var setting = new AzureTableSettings(storageAccount, storageKey, tableName);
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
    }
}
