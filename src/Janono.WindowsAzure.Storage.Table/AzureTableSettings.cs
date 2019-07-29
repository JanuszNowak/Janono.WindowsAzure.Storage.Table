namespace Janono.WindowsAzure.Storage.Table
{
    using System;

    public class AzureTableSettings
    {
        public AzureTableSettings(string storageAccount, string storageKey, string tableName)
        {
            if (string.IsNullOrEmpty(storageAccount))
                throw new ArgumentNullException("storageAccount");

            if (string.IsNullOrEmpty(storageKey))
                throw new ArgumentNullException("storageKey");

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            this.StorageAccount = storageAccount;
            this.StorageKey = storageKey;
            this.TableName = tableName;
        }

        public string StorageAccount { get; }
        public string StorageKey { get; }
        public string TableName { get; }
    }
}
