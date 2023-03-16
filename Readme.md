#General Overview of OfficeToolkit Function App.

In modern workplaces, business owners and executives working in a Microsoft Shop environment are always working with spreadsheets. More specifically, in Financial Services and investment banking, executives handle large amounts of sensitive data in different excel formats and in most cases generate files that are over 100mb in size.

Most modern workplaces tend to automate the process of interacting with these large files, processing and manipulating data in a secure way while minimizing human errors. Most times business development staff use low code tools like Azure Logic Apps to extract large business files from a SharePoint site and move them to storage containers where Scripts that were written to process them with adequate permissions gain access to interact and execute business logic on them.

The problem today on Microsoft Azure with Low Code tools like Azure Logic Apps is that they cannot move files that are over a 100mb in size, there is a hard limit. Today, I have decided to share an ASP.NET function app that solves this problem for many modern workplaces and businesses running a Microsoft Shop!

The solution I have today is an Azure HTTP triggered function app which accepts three parameters in the POST request BODY.

sitePath (The Absolute Path to the SharePoint Site's folder where the file was saved)
siteId (The site Id for the SharePoint Site hosting the file)
fileName (The filename of the file to download)
Once the right payload is passed to the invocation of the HTTP Service, it processes the request and uses the Microsoft Graph SDK to download the large file (this was tested with a file of 250mb) from the SharePoint folder and uploads it to an Azure blob storage whose URI settings should be supplied in the app configuration file explained below.

To run this function app successfully in your Tennant, you will need to supply specific values to your local.appsetting.json file. The Keys the app is expecting for these values are listed below and quite self-explanatory.

"Values": {
"AzureWebJobsStorage__blobServerUri": "",
"CwdToolBox:KeyVaultUri": "https://",
"CwdToolBox:SharePoint:ApiSettings:KeyvaultSecret_TenantId": "",
"CwdToolBox:SharePoint:ApiSettings:KeyvaultSecret_ClientId": "",
"CwdToolBox:SharePoint:ApiSettings:KeyvaultSecret_CllientSecret": ""
}

The above details are basically your Azure Blob Storage URI, Azure KeyVault URI, The Azure Tenant ID, Client Id and Client Secret for OAuth token retrieval used to access the Graph API. You should add the Tenant ID, Client Id and Client Secret to your Azure Key Vault with these keys:

KeyvaultSecret_TenantId
KeyvaultSecret_ClientId
KeyvaultSecret_ClientSecret
The function app uses Azure.Identity.DefaultAzureCredentail to provide default TokenCredential authentication flow for Azure Blob Service Client when connected to an Azure Subscription or when deployed to Azure.

I hope this toolkit will help millions of businesses who cannot automate past the hard limits that low code tools are facing today on Azure.