using System;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;

namespace TestSDK
{
    class Program
    {
        //PROBLEMAS COM A UTILIZAÇÃO DA LICENSA EM OUTROS COMPUTADORES
       
        static string keyName = "images/video.avi";
        //static string filePath = @"C:\Isaias\AWS\AWSTestSDK\TestSDK\amazon.png";

        static string filePath = @"C:\Users\Isaías\Pictures\Saved Pictures\video.avi";
        static string bucketName = "isaias-bucket";
        static IAmazonS3 client;

        public static void Main(string[] args)
        { 
            using (client = new AmazonS3Client(Amazon.RegionEndpoint.SAEast1))
            {
                Console.WriteLine("************************ SISTEMA DE GERÊNCIAMENTO DE BUCKETS ************************");


               // CreatingBucket("Bucket-Name");

                ListingBuckets();

             //   UploadingObject();

             //   DeletingObject(bucketName, keyName);

                ListingObjects();

                //   DeletingBucket(bucketName);

                //   CreatingBucket(bucketName);

               

                Downloading();

                try
                {
                    Console.WriteLine("Retrieving (GET) an object");
                    string data = ReadObjectData();
                    Console.WriteLine("\n" + data.ToString());
                }
                catch (AmazonS3Exception s3Exception)
                {
                    Console.WriteLine(s3Exception.Message,
                                      s3Exception.InnerException);
                }



            }         

            Console.WriteLine("Pressione alguma tecla para terminar...");

           
            Console.ReadKey();
        }


        static void CreatingBucket(String bucketName)
        { 
            client.PutBucket(bucketName);
            Console.WriteLine("Criando bucket: {0} ...", bucketName);
          
        }

        //Só deleta se o Bucket estiver vazio
        static void DeletingBucket(String bucketName)
        {
            client.DeleteBucket(bucketName);
            Console.WriteLine("Deletando bucket: {0} ...", bucketName);

        }

        static void ListingBuckets()
        {
            Console.WriteLine("\nListando os Buckets:");
            // Issue call
            ListBucketsResponse response = client.ListBuckets();

            // View response data
            Console.WriteLine("Criador dos Buckets: - {0}", response.Owner.DisplayName);
            foreach (S3Bucket bucket in response.Buckets)
            {
                Console.WriteLine("Bucket Nome: {0}, Criado em {1}", bucket.BucketName, bucket.CreationDate);
            }

        }

        static void ListingObjects()
        {
            Console.WriteLine("\nListando todos os objetos do Bucket: {0} ...", bucketName);
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = 10
                };
                ListObjectsV2Response response;
                do
                {
                    response = client.ListObjectsV2(request);

                    // Process response.
                    foreach (S3Object entry in response.S3Objects)
                    {
                        Console.WriteLine("nome = {0} tamanho = {1}",
                            entry.Key, entry.Size);
                    }
                    //Console.WriteLine("Next Continuation Token: {0}", response.NextContinuationToken);
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated == true);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                    "To sign up for service, go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                     "Error occurred. Message:'{0}' when listing objects",
                     amazonS3Exception.Message);
                }
            }
        }

        static void UploadingObject()
        {
            Console.WriteLine("\nEnviando Objeto: {0} para o Bucket: {1} ...", keyName, bucketName);

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = keyName,
                FilePath = filePath
            };
            PutObjectResponse response2 = client.PutObject(request);


        }

        static void DeletingObject(String bucketName, String nomeArquivo)
        {
            Console.WriteLine("\nDeletando o Objeto: {0} do Bucket: {1} ...", nomeArquivo, bucketName);

            client.DeleteObject(bucketName, nomeArquivo);

        }

        static string ReadObjectData()
        {
            string responseBody = "";

            using (client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1)) 
            {
                GetObjectRequest request = new GetObjectRequest 
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = client.GetObject(request))  
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    Console.WriteLine("The object's title is {0}", title);

                    responseBody = reader.ReadToEnd();
                }
            }
            return responseBody;
        }

        static void Downloading()
        {

            
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            using (GetObjectResponse response = client.GetObject(request))
            {
                string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
                if (!File.Exists(dest))
                {
                    response.WriteResponseStreamToFile(dest);
                }
            }
            
        }
    }
}
