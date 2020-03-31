# GisMeteoParser

GisMeteoParser - system that will provide weather information in the selected city for tomorrow. 

The system consists of:  
- MySql database (stores weather information. Default server=’localhost’, user=’root’, password=’root’) 
- The parser (grabber) of the site http://www.gismeteo.ru/ (a console application that periodically takes weather data and stores it in a database; the grabber takes weather data for all cities is presented on the main page of the site) 
- WCF service (provides customers with data on the weather, the layer between the client application and the database) 
- WPF client application (displays weather information in the selected city for tomorrow; the client application interacts only with the WCF service) 

## Deployment

System require Mysql DB on localhost with user='root' and password='root'

## Built With

* [MySQL.Data] -  DB conncetion
* [HtmlAgillityPack] - Web grabber
* [TaskScheduler] - Used to create system task for weather updates
* [Extended.Wpf.Toolkit] - Additional UI elements

## Authors

* Dima Dovg
