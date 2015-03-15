ieee-checkin
============

Swipe your U Card or enter your info to check in at IEEE UMN meetings.

## Dependencies
* Currently running [twitter bootstrap](http://getbootstrap.com/2.3.2/), [jquery](http://jquery.com/download/)
* MySQL database
* Google Drive .NET SDK, Google Sheets .NET SDK, Google OAuth .NET SDK (for IEEECheckin.ASPDocs)

## Versions
* IEEECheckin.ASPDocs - ASP.NET version, uses IndexedDB for client side storage, Google APIs for Google Drive integration, and MySQL for formats and regex

## Install
.NET (IEEECheckin.ASP & IEEECheckin.ASPDocs):
* Use the WebPublish extension in Visual Studio to publish to an IIS/ASP.NET server

## Usage
* Fill in the Web.config database connection to point to a MySQL database instance
* Fill in the Web.config Google Client Id and Secret with you Google API keys
* Use the SQL script provided to create the needed tables
* Deploy to an IIS server with ASP.NET 4.0 configured
