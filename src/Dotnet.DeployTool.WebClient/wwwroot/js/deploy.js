"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/deployHub").build();


connection.on("Feedback", function (feedback) {
    var msg = feedback.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = msg;
    //var li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);

    document.getElementById("textarea").value += '\n' + encodedMsg

    document.getElementById("textarea").scrollTop = document.getElementById("textarea").scrollHeight 
});

//Disable send button until connection is established
document.getElementById("installButton").disabled = true;

document.getElementById("installButton").addEventListener("click", function (event) {
    var osSelect = document.getElementById("runtimes");
    var osValue = osSelect.options[osSelect.selectedIndex].value;

    var runtimeSelect = document.getElementById("runtimes");
    var runtimeValue = runtimeSelect.options[runtimeSelect.selectedIndex].value;

    connection.invoke("InstallAppRuntime", runtimeValue, osValue).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//Disable send button until connection is established
document.getElementById("uploadButton").disabled = true;

document.getElementById("uploadButton").addEventListener("click", function (event) {

    var solutionPath = document.getElementById("solutionPath").value;

    connection.invoke("UploadSolution", solutionPath).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//Disable send button until connection is established
document.getElementById("testConnectionButton").disabled = true;

document.getElementById("testConnectionButton").addEventListener("click", function (event) {

    var ip = document.getElementById("ip").value;
    var port = document.getElementById("port").value; 
    var username = document.getElementById("username").value; 
    var pemKeyPath = document.getElementById("pemKeyPath").value; 

    connection.invoke("UpdateConfigAndTestConnection", pemKeyPath, ip, port, username).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


connection.start().then(function () {
    document.getElementById("installButton").disabled = false;
    document.getElementById("uploadButton").disabled = false;
    document.getElementById("testConnectionButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});



// TODO: Some kind of statemanagment, where cookies remembers old stuff and preloads Pemkey, ip, port and so on, so user doesn't have to pick it each time.