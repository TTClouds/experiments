import pulsar
import sys
import json

WORD_COUNT_TOPIC = 'wordcount'
SUBSCRIPTION = 'my-sub'
TIMEOUT = 10000

def main(args):
    # Create a pulsar client instance with reference to the broker
    client = pulsar.Client('pulsar://localhost:6650')

    consumer = client.subscribe(WORD_COUNT_TOPIC, SUBSCRIPTION)
    while True:
        try:
            # try and receive messages with a timeout of 10 seconds
            msg = consumer.receive(timeout_millis=TIMEOUT)
            print("Received message: %s" % msg.data())
            consumer.acknowledge(msg)  # send ack to pulsar for message consumption
        except Exception:
            print("No message received in the last 10 seconds")

    client.close()

if __name__ == '__main__':
    main(sys.argv)
