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
&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
&lt;nlog xmlns=&quot;http://www.nlog-project.org/schemas/NLog.xsd&quot;
      xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;
      internalLogToConsole=&quot;true&quot;
      internalLogLevel=&quot;Debug&quot;&gt;

  &lt;extensions&gt;
    &lt;add assembly=&quot;NLog.Targets.AmazonSES&quot;/&gt;
  &lt;/extensions&gt;

  &lt;targets&gt;
    &lt;!-- Log individual messages through Amazon SES --&gt;
    &lt;target name=&quot;ses&quot; xsi:type=&quot;SESTarget&quot;
            awsAccessKey=&quot;Paste AWS Access Key here&quot;
            awsSecretKey=&quot;Paste AWS Security Key here&quot;
            from=&quot;mail@somewhere.com&quot;
            to=&quot;mail@somewhere.com&quot;
            subject=&quot;Application error&quot;
            layout=&quot;${time} - ${level} - ${message}&quot;/&gt;
  &lt;/targets&gt;

  &lt;rules&gt;
    &lt;logger name=&quot;*&quot; minlevel=&quot;Error&quot; writeTo=&quot;ses&quot; /&gt;
  &lt;/rules&gt;
&lt;/nlog&gt;
</pre>
