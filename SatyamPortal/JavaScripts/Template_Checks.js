function checkNoFiles(connectionString, containerName, directoryName) {

    var request = connectionString + "," + containerName + "," + directoryName;
    $.ajax({
        type: "POST",
        url: "WebServiceHelpers.aspx/getNoFilesInAzureLocation",
        data: "{'request': '" + request + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: azureTemplateCheckResult,
        failure: function (response) {
            alert(response.d);
        }
    });
}

function checkAmazonMoney(accessKey, secretAccessKey) {

    var request = accessKey + "," + secretAccessKey;
    $.ajax({
        type: "POST",
        url: "WebServiceHelpers.aspx/checkMoneyInAmazonAccount",
        data: "{'request': '" + request + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: amazonTemplateCheckResult,
        failure: function (response) {
            alert(response.d);
        }
    });
}

function azureTemplateCheckResult(response)
{
    var no = parseInt(response.d);
    if (no == -1)
    {
        alert('Problem : The Azure Blob Storage Info is Invalid!');
    }
    else
    {
        if (no == 0) {
            alert('Problem : There are no files in this Azure Storage location!');
        }
        else {
            alert('Success : There are ' + no + ' files at your Azure Storage location.');
        }
    }
}


function amazonTemplateCheckResult(response) {
    var money = parseFloat(response.d);
    if (money < 0 ) {
        alert('Problem : The Amazon Account Info is Invalid!');
    }
    else {
        if (money < 0.1) {
            alert('Problem : There is not enough money!');
        }
        else {
            alert('Success : There is ' + money + '$ in the account');
        }
    }
}

function Template_Azure_Location_Check()
{
    var azure_connectionString_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_AzureBlobStorageConnectionStringTextBox");
    var azure_connectionString = azure_connectionString_tb.value;

    var azure_container_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_AzureBlobStorageContainerNameTextBox");
    var azure_container = azure_container_tb.value;

    var azure_directory_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_AzureBlobStorageContainerDirectoryNameTextBox");
    var azure_directory = azure_directory_tb.value;

    if (azure_connectionString == "") {
        alert('Problem : Azure Storage Blob Connection String Field is Empty');
        return false;
    }
    if (azure_container == "") {
        alert('Problem : Azure Storage Blob Container Field is Empty');
        return false;
    }

    checkNoFiles(azure_connectionString, azure_container, azure_directory);
    return false;
}

function Template_Amazon_Information_Check()
{
    var amazon_accessKeyID_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_AmazonAccessKeyID");
    var amazon_accessKeyID = amazon_accessKeyID_tb.value;

    var amazon_secretAccessKey_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_AmazonSecretAccessKeyID");
    var amazon_secretAccessKey = amazon_secretAccessKey_tb.value;

    if (amazon_accessKeyID == "") {
        alert('Amazon access Key ID is Empty');
        return false;
    }
    if (amazon_secretAccessKey == "") {
        alert('Amazon Secret Access Key Id is Empty');
        return false;
    }

    checkAmazonMoney(amazon_accessKeyID, amazon_secretAccessKey);
    return false;
}

function CategoryCheck()
{
    var azure_container_lb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_CategoryListBox");
    if (azure_container_lb.options.length == 0)
    {
        alert('You have no categories!');
        return false;
    }
    return true;
}


function Price_Check() {
    var azure_price_tb = document.getElementById("MainTemplatePlaceHolder_ContentPlaceHolder1_PriceTextBox");
    var priceTxt = azure_price_tb.value;
    if (isNaN(priceTxt) || priceTxt=="") {
        alert('Please enter a valid price');
        return false;
    }
    var price = parseFloat(priceTxt);
    if (price <= 0) {
        alert('Please enter a valid price');
        return false;
    }
    return confirm('Are you sure the price is ' + price);
}

function Template_SingleObjectLabeling_SubmitCheck()
{

    if (!Template_Azure_Location_Check())
    {
        return false;
    }

    if (!Template_Amazon_Information_Check())
    {
        return false;
    }

    if (!CategoryCheck())
    {
        return false;
    }
    if (!Price_Check())
    {
        return false;
    }
    return confirm('Are you sure you want to Submit?');
}

function Template_MultiObjectLocalizationAndLabeling_SubmitCheck() {

    if (!Template_Azure_Location_Check()) {
        return false;
    }
    if (!Template_Amazon_Information_Check()) {
        return false;
    }
    if (!CategoryCheck()) {
        return false;
    }
    if (!Price_Check()) {
        return false;
    }
    return confirm('Are you sure you want to Submit?');
}

function Template_MultiObjectTracking_SubmitCheck() {

    if (!Template_Azure_Location_Check()) {
        return false;
    }
    if (!Template_Amazon_Information_Check()) {
        return false;
    }
    if (!CategoryCheck()) {
        return false;
    }
    if (!Price_Check()) {
        return false;
    }
    return confirm('Are you sure you want to Submit?');
}

function onAmazonMechanicalTurkCheckBoxChanged()
{
    alert('checkbox clicked!!');
    return false;
}