using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services
{
    internal class AzureQueueService
    {
        private static AzureQueueService _instance;
        private CloudStorageAccount _storageAccount;
        private CloudQueueClient _queueClient;

        private AzureQueueService()
        {
            // Retrieve storage account from connection string.
            _storageAccount = CloudStorageAccount.Parse(
                AppConfig.Instance.GetValue("AzureStorageConnectionString"));

            // Create the queue client.
            _queueClient = _storageAccount.CreateCloudQueueClient();
        }

        public static AzureQueueService Instance
        {
            get {
                if(_instance == null)
                {
                    _instance = new AzureQueueService();
                }

                return _instance;
            }
        }

        public void AddMessage(string queueName, string message)
        {
            // Retrieve a reference to a container.
            CloudQueue queue = _queueClient.GetQueueReference(queueName.ToLower());

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            queue.AddMessage(new CloudQueueMessage(message));
        }
    }
}
