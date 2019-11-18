<%@ Page Title="" Language="C#" MasterPageFile="~/MainPage.Master" AutoEventWireup="true" CodeBehind="LandingPage.aspx.cs" Inherits="SatyamPortal.LandingPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainTemplatePlaceHolder" runat="server">
    <p>
    Satyam is a system that allows you to obtain ground truth data for your machine learning tasks with ease. It allows you to obtain ground truth for machine vision tasks using crowdsourcing without going through the pains of creating a website for the task, or managing the crowdsourcing workers. You simply select one of the templates that most suits you and customize it for your task and submit. The results then appear in your azure blob in a json file format in two days!</p>
<p>
    <strong>How Satyam Works: </strong>Behind the scenes, Satyam creates and presents a website to the crowdsourcer in a tested UI, customized to your needs. Each task is given to multiple&nbsp; crowdsourcers to ensure quality and intelligent algorithms determine dynamically if more crowdsourcers are required to ensure quality. Satyam is uses smart algorthms to determine which crowdsourcers have done the task accurately and should be paid and pays them while automatically rejecting others. Satyam also determine an optimal price for your task by dynamically monitoring the difficulty and response of the crowdsourcers.</p>
<p>
    <strong>What you Need to Use Satyam : </strong>i) You will need an amazon turk requester account with money in it to pay the turkers ii) You will need an Azure blob storage account to store your images and videos for which you require ground truth and also where the ground-truth results will be reported.</p>
<p>
&nbsp;</p>
</asp:Content>
