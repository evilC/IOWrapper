#include <Servo.h>

#include "ArduinoDescriptor.pb.h"
#include "pb.h"
#include "pb_encode.h"
#include "pb_decode.h"

#define AXIS_MAX 32767
#define AXIS_MIN -32768
#define SERVO_RANGE 364


Servo myservo;  // create servo object to control a servo
arduino_ArduinoDescriptor descriptor = {};
pb_istream_s pb_in;

const int DEBUGPIN = 2;
const char C_DISCOVER = 'D';


void setup() {
  Serial.begin(500000);
  myservo.attach(16);  // attaches the servo on pin 9 to the servo object

  pinMode(DEBUGPIN, OUTPUT);
  
  myservo.write(180);
  pb_in = as_pb_istream(Serial);
}

void loop() {
  read_descriptor();

  myservo.write(axis_to_servo(descriptor.axis[0]));
}

int axis_to_servo(int axis){
  return (axis - AXIS_MIN) / SERVO_RANGE;
}

static bool pb_stream_read(pb_istream_t *stream, pb_byte_t *buf, size_t count) {
    Stream* s = reinterpret_cast<Stream*>(stream->state);
    size_t written = s->readBytes(buf, count);
    return written == count;
};

pb_istream_s as_pb_istream(Stream& s) {
    return {pb_stream_read, &s, SIZE_MAX, 0};
};

void read_descriptor(){
  if (!Serial.available()) return;

  arduino_ArduinoDescriptor message = {};

  digitalWrite(DEBUGPIN, HIGH);
  if (pb_decode_delimited(&pb_in, arduino_ArduinoDescriptor_fields, &message)){
    descriptor = message;
  }

  Serial.write(C_DISCOVER);
  digitalWrite(DEBUGPIN, LOW);
}
