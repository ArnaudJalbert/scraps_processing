using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace SaveScreenshot
{
    public class ScreenshotUploader : MonoBehaviour
    {
        public GameObject ui;
        private string serverURL = "https://scraps-processing-api-delicate-pond-5077.fly.dev/upload"; // Replace with your Flask server URL

        public void UploadScreenshot(string timestamp)
        {   
            ui.SetActive(false);
            StartCoroutine(SendScreenshot(timestamp));
            
        }

        IEnumerator SendScreenshot(string timestamp)
        {
            // Capture a screenshot
            yield return new WaitForEndOfFrame(); 
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();
            
            byte[] imageBytes = texture.EncodeToPNG();
            yield return new WaitForSeconds(1); // Wait for the screenshot to be saved

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", imageBytes, timestamp + ".png", "image/png");

            UnityWebRequest request = UnityWebRequest.Post(serverURL, form);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error uploading image: " + request.error);
            }
            else
            {
                Debug.Log("Image uploaded successfully!");
            }
            ui.SetActive(true);
        }
    }
}
