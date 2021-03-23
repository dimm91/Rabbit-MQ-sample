# Work Queues
The main idea behind Work Queues (aka: Task Queues) is to avoid doing a resource-intensive task immediately and having to wait for it to complete. Instead we schedule the task to be done later

We can reuse the previous project from `Simple-Producer-Consumer`. We can simulate a long task with putting the `Thread.Sleep(1000);` on the received event.

## [Message acknowledgment](https://www.rabbitmq.com/confirms.html)

If one of the consumers starts a long task and dies with it only partly done. With our current code, once RabbitMQ delivers a message to the consumer it immediately marks it for deletion. In this case, if you kill a worker/consumer we will lose the message it was just processing. We'll also lose all the messages that were dispatched to this particular worker but were not yet handled.

For this issue we need to make sure a message is never lost. An ack(nowledgement) is sent back by the consumer to tell RabbitMQ that a particular message has been received, processed and that RabbitMQ is free to delete it.

Therefore if a consumer dies without sending an `ack`, RabbitMQ will understand that a message wasn't processed fully and will re-queue it. If there are other consumers online at the same time, it will then quickly redeliver it to another consumer. That way you can be sure that no message is lost, even if the workers occasionally die.

```csharp
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    var bodyArray = e.Body.ToArray();//Byte array of message
    var message = Encoding.UTF8.GetString(bodyArray);

    Console.WriteLine($"[{DateTime.UtcNow}] - Message received: " + message);
    Thread.Sleep(5000);
    // Manually acknowledge that the message was received and procesed correctly.
    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
};

// https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
// We set the 'autoAck' to false, since we will to the acknowledge of the message manually
channel.BasicConsume(queueName, false, consumer);
```

## Dispatch

You might see that dispatching still behaves a little weird, one worker will be constantly busy and the other one will do hardly any work. This is because RabbitMQ just dispatches a message when the message enters the queue, it doesn't look at the number of unacknowledged messages for a consumer

In order to change this behavior we can use the `BasicQos` method with the `prefetchCount = 1` setting. This tells RabbitMQ not to give more than one message to a worker at a time