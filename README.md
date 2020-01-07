# COTWOCRConsole
This repository mantains the Visual Studio solution used to build the COTWOCRConsole application that works as companion to track harvests during [theHunter COTW game](http://callofthewild.thehunter.com/en) :)

This is what it looks like:

![COTWOCRConsole](https://cdn.discordapp.com/attachments/414084461706215424/662368677387305025/unknown.png)

This application relies on [Amazon Textract service](https://aws.amazon.com/textract/) to retrieve the data from the images and [Microsoft Azure Blob Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction) to store them in the cloud.

To make it work, you have to add the required values in the appsettings.json file as shown below and move it to Windows Local App Data folder (e.g.: C:\Users\JohnDoe\AppData\Local)

```
{
  "StorageService": {
    "ConnectionString": "This is the connection string that Azure Storage provides"
  },
  "OCRService": {
    "AccessKey": "Access key provided by AWS",
    "SecretAccessKey": "Secret key provided by AWS",
    "Region": "AWS region where the service was deployed"
  }
}
```

To start the application you must provide two arguments:

- hunterid: It will be used to create a container in the blob storage and store all the images below it; (e.g.: johndoe)
- folder: This is the folder where the images are generated when capturing screenshots while gaming. (e.g.: C:\Program Files (x86)\Steam\userdata\987654321\760\remote\518790\screenshots)

Fire a new prompt window and run:

COTWOCRConsole --hunterid johndoe --folder "C:\Program Files (x86)\Steam\userdata\987654321\760\remote\518790\screenshots"

Happy hunting! :)
