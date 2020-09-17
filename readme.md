# Description

This project with implement examples with Retry and Circuit Breaker resilience patterns using the Polly lib.

--- 

# Vaurien
Vaurien, the Chaos TCP Proxy

Ever heard of the Chaos Monkey?

It’s a project at Netflix to enhance the infrastructure tolerance. The Chaos Monkey will randomly shut down some servers or block some network connections, and the system is supposed to survive to these events. It’s a way to verify the high availability and tolerance of the system.

Besides a redundant infrastructure, if you think about reliability at the level of your web applications there are many questions that often remain unanswered:

What happens if the MYSQL server is restarted? Are your connectors able to survive this event and continue to work properly afterwards?
Is your web application still working in degraded mode when Membase is down?
Are you sending back the right 503s when postgresql times out ?
Of course you can – and should – try out all these scenarios on stage while your application is getting a realistic load.

But testing these scenarios while you are building your code is also a good practice, and having automated functional tests for this is preferable.

That’s where Vaurien is useful.

Vaurien is basically a Chaos Monkey for your TCP connections. Vaurien acts as a proxy between your application and any backend.

You can use it in your functional tests or even on a real deployment through the command-line

## Installing Vaurien

You can install Vaurien directly from PyPI; the best way to do so is via pip:

>$ pip install vaurien

## Using Vaurien from the command-line

> $ vaurien --local localhost:8000 --distant google.com:80 --behavior 20:delay