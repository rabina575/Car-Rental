using Firebase.Storage;
using System.Diagnostics;

namespace HajurKoCarRental.Utils
{
    public class FirebaseStorageHelper
    {
        private readonly FirebaseStorage _firebaseStorage;

        public FirebaseStorageHelper()
        {
            //initialising firebase with our storage bucket
            _firebaseStorage = new FirebaseStorage("hajurkocarrental-d8788.appspot.com");
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            var task = "";
            try
            {
                //method to update our files to firebase
                task = await _firebaseStorage.Child(fileName).PutAsync(fileStream);
            }
            catch (Exception ex)
            {
                Trace.TraceError("An error occurred: {0}", ex.Message);
            }
            return task;
        }
    }
}
