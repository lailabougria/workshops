# Designing complex business processes with messaging

## Abstract

Messaging in distributed systems offers a range of valuable benefits: it enables the design of decoupled components, where each component can evolve and scale independently. In addition to improving the system’s fault tolerance, it opens the door to building truly self-healing systems. But there's more to explore; the true power of messaging surfaces when we start expressing complex business processes with intention-revealing messages (both commands and events) to reflect what's happening in the real world. These business processes are usually non-trivial, exceed the boundaries of a single method- or service invocation, and require us to redefine certain concepts, including time.

If you've been exposed to the basic building blocks of messaging and want to transition your system from a collection of meaningless and often highly-coupled service-to-service calls to one composed of carefully designed processes that reflect your business use cases, this workshop is for you.

Throughout this workshop, you’ll learn:

- How to identify complex business processes in your domain
- Effective techniques to help design these processes
- What orchestration and choreography are, but more importantly, when and why to use each pattern
- How to deal with actions that occur at different points in time
- How to deal with out-of-order events seamlessly
- How to test complex, long-running processes and troubleshoot issues where it matters most: in production!

To get the most out of this workshop, basic messaging knowledge is expected due to the limited time available. This includes understanding message brokers (like Azure Service Bus, RabbitMQ, or other broker technologies) and messaging patterns like Request-Reply and Publish-Subscribe. 

Throughout this workshop, we'll dive into different design exercises in small groups, where you'll mostly need pen and paper. Code examples and exercises will be provided as additional material to showcase the use cases discussed during the workshop.

Join me and unlock the secrets of messaging's real-world superpowers!
