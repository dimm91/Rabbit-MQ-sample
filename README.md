# Rabbit-MQ-sample

Sample project to implement a message queue with Rabbit Mq

## Requirements

It is required to install the Rabbitmq docker image
```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
 You can also install the image without the management tag.