# Phobos® Code Samples

![Phobos™ logo](phobos_logo.png)

This repository contains samples projects for working with [Phobos: DevOps Suite for Akka.NET](https://phobos.petabridge.com/) and is designed to showcase how some of the custom telemetry APIs work.

### Phobos.Samples.Filtering

This sample is intended to help show the whitelisting feature in Phobos tracing. This sample will tell Phobos to only collect traces for messages with the IStatus interface. This will limit the messages we are seeing to only show when a customer places an order and the supply store does not have enough stock to supply the order. When an order is placed and there is not enough stock, the supply store will let the customer Actor know. The customer Actor will then forward this to the manager which will then restock the particular item. 
This sample is configured to be be used with App insights, you can find more information on how to get this setup [here](https://phobos.petabridge.com/articles/tracing/backends/app-insights.html). However you can change the tracing engine to any of our supported implementations listed in our [Phobos website](https://phobos.petabridge.com/articles/tracing/).

## License
Phobos is a commercial product and [its end-user license agreement can be found here](https://phobos.petabridge.com/articles/setup/license.html).

The sources in this repository, however, is open source and is [licensed under the terms of Apache 2.0](LICENSE).

This library is maintained by [Petabridge](https://petabridge.com/)®. Copyright 2015-2018.