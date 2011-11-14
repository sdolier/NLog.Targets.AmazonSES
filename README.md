Amazon SES Target for NLog
==========================
Amazon SES Target for [NLog 2.0](http://nlog-project.org/) allows you to send log messages through the [Amazon Simple Email Service](http://aws.amazon.com/ses/).

Suites .net applications hosted in Amazon EC2.

### Requirements ###

1. [Amazon AWS](http://aws.amazon.com/) account credentials
2. [Amazon Simple Email Service](http://docs.amazonwebservices.com/ses/latest/GettingStartedGuide/) enabled with a verified sender emaill address

### Configuration ###
Include NLog.Targets.AmazonSES.dll to your project and add NLog.Targets.AmazonSES as an extension in your NLog.config.

<pre>
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      internalLogToConsole="true"
      internalLogLevel="Debug">

  <extensions>
    <add assembly="NLog.Targets.AmazonSES"/>
  </extensions>

  <targets>
    <!-- Log individual messages through Amazon SES -->
    <target name="ses" xsi:type="SESTarget"
            awsAccessKey="Paste AWS Access Key here"
            awsSecretKey="Paste AWS Security Key here"
            from="mail@somewhere.com"
            to="mail@somewhere.com"
            subject="Application error"
            layout="${time} - ${level} - ${message}"/>
  </targets>

  <rules>
    <logger name="*" minlevel="Error" writeTo="ses" />
  </rules>
</nlog>
</pre>
