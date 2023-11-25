using MongoDB.Driver;

namespace Schedule_Mate.Models
{
    public class MongoDBServices
    {
        MongoClient client;
        public IMongoDatabase database;
        public MongoDBServices()
        {
            // Establishing connection with MongoDB
            //Replace this link with your mongodb connection string or for local host comment this line and uncomment the next line and uncomment the next line and uncomment the next line.
            client = new MongoClient("mongodb+srv://<username>:<password>@sandbox.tn5uz9e.mongodb.net/?retryWrites=true&w=majority");
            // client=new MongoClient("mongodb://localhost:27017/");

            //Getting database object
            database = client.GetDatabase("scheduleMateDb");
        }
    }
}