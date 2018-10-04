Contains the IOController

Responsibilities include:
* Acts as an interface between the Providers and the front-end.  
The front-end does not directly call Providers - all calls to Providers route through the IOController.  
* Ensuring that one `SubscriberGuid` is only ever subscribed to one Provider at a time.
* Loading the Provider DLLs