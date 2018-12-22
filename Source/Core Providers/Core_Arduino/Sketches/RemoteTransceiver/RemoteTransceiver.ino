#include <Servo.h>
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
#include "DFRobotDFPlayerMini.h"

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
pb_istream_s radio_pb_in;
pb_ostream_s radio_pb_out;

const int DEBUGPIN = 17;
const int SERVERPIN = 2;
const int CE_PIN = 9;
const int CSN_PIN = 15;
const char C_DISCOVER = 'D';

RF24 radio(CE_PIN, CSN_PIN); // CE, CSN
const byte address[6] = "00001";
const byte frame_start = 0x03;

DFRobotDFPlayerMini myDFPlayer;


void setup() {
  Serial.begin(500000);
  Serial1.begin(9600);

  pinMode(DEBUGPIN, OUTPUT);
  digitalWrite(DEBUGPIN, HIGH);
  pinMode(SERVERPIN, INPUT);
  digitalWrite(SERVERPIN, HIGH);
  
  pb_in = as_pb_istream(Serial);

  if (is_server()){
    radio.begin();
    radio.openWritingPipe(address);
    radio.setPALevel(RF24_PA_MAX);
    radio.stopListening();
  } else {
    radio.begin();
    radio.openReadingPipe(0, address);
    radio.setPALevel(RF24_PA_MAX);
    radio.startListening();

    myservo.attach(16);  // attaches the servo on pin 9 to the servo object

    if (myDFPlayer.begin(Serial1)) {
      myDFPlayer.volume(25);
    }

  }
}

void loop() {
  if (is_server()){
    receive_descriptor();
    send_radio_message();
  } else {
    receive_radio_message();
    myservo.write(axis_to_servo(descriptor.axis[0]));
    if (descriptor.button[0] != 0){
      myDFPlayer.play(1);
    }
  }
  
  //digitalWrite(DEBUGPIN, val % 2 == 0 ? LOW : HIGH);
  
}

void send_radio_message(){
  uint8_t buffer[arduino_ArduinoDescriptor_size]; 
  pb_ostream_t ostream = pb_ostream_from_buffer(buffer, sizeof(buffer));  
  pb_encode(&ostream, arduino_ArduinoDescriptor_fields, &descriptor);
  size_t num_written = ostream.bytes_written;
  
  radio.write(&frame_start, sizeof(byte));
  radio.write(&num_written, sizeof(size_t));
  radio.write(&buffer, num_written);
}

void receive_radio_message(){
  byte framebyte;
  while (radio.available()) {
    radio.read(&framebyte, sizeof(byte));
    if (framebyte != frame_start) continue;

    digitalWrite(DEBUGPIN, HIGH);
    
    while(!radio.available()){}

    arduino_ArduinoDescriptor message = {};
    
    size_t len;
    radio.read(&len, sizeof(size_t));
    while(!radio.available()){}
    
    uint8_t buffer[len]; 
    radio.read(&buffer, len);
    pb_istream_t istream = pb_istream_from_buffer(buffer, len); 

    if(pb_decode(&istream, arduino_ArduinoDescriptor_fields, &descriptor)){
      descriptor = message;
    }
    break;
  }
  digitalWrite(DEBUGPIN, LOW);
}

void receive_descriptor(){
  if (!Serial.available()) return;

  arduino_ArduinoDescriptor message = {};

  digitalWrite(DEBUGPIN, HIGH);
  if (pb_decode_delimited(&pb_in, arduino_ArduinoDescriptor_fields, &message)){
    descriptor = message;
  }

  Serial.write(C_DISCOVER);
  digitalWrite(DEBUGPIN, LOW);
}

int axis_to_servo(int axis){
  return (axis - AXIS_MIN) / SERVO_RANGE;
}

static bool is_server(){
  return digitalRead(SERVERPIN) == HIGH;
}

static bool pb_stream_read(pb_istream_t *stream, pb_byte_t *buf, size_t count) {
    Stream* s = reinterpret_cast<Stream*>(stream->state);
    size_t written = s->readBytes(buf, count);
    return written == count;
};

static bool pb_print_write(pb_ostream_t *stream, const pb_byte_t *buf, size_t count) {
    Print* p = reinterpret_cast<Print*>(stream->state);
    size_t written = p->write(buf, count);
    return written == count;
};

pb_istream_s as_pb_istream(Stream& s) {
    return {pb_stream_read, &s, SIZE_MAX, 0};
};

pb_ostream_s as_pb_ostream(Print& p) {
    return {pb_print_write, &p, SIZE_MAX, 0};
};
