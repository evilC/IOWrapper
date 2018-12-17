#include <Servo.h>

#include "ArduinoDescriptor.pb.h"
#include "pb.h"
#include "pb_encode.h"
#include "pb_decode.h"

Servo myservo;  // create servo object to control a servo
arduino_ArduinoDescriptor message = {};
pb_istream_s pb_in;

const int DEBUGPIN = 2;

int pos = 0;    // variable to store the servo position

// Pins
const int RED = 9; // 23;
const int YELLOW = 6; // 22;
const int GREEN = 11; // 21;

const int PEDRED = 10; // 9;
const int PEDGREEN = 3; // 10;

const int BUTTON = 2;
const int PIEZO = 12;

// Commands
const char C_LIGHTS = 'T';
const char C_HEARTBEAT = 'H';
const char C_RESET = 'R';
const char C_SPEAKER = 'S';
const char C_DISCOVER = 'D';
const char C_BRIGHTNESS = 'B';

// States (0 off, 1 on, 2 flashing)
char lights[6];
unsigned long lastUpdate;
static boolean connected = false;
int brightness = 10;

void setup() {
  Serial.begin(500000);
  myservo.attach(16);  // attaches the servo on pin 9 to the servo object

  pinMode(DEBUGPIN, OUTPUT);
  
  myservo.write(180);
  pb_in = as_pb_istream(Serial);
}

void loop() {
  //get_input();

  //updateLights();
  read_descriptor();

  if (message.button != 0){
    myservo.write(90);
  } else {
    myservo.write(1);
  }
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

  digitalWrite(DEBUGPIN, HIGH);
  pb_decode_delimited_noinit(&pb_in, arduino_ArduinoDescriptor_fields, &message);

  Serial.write(C_DISCOVER);
  digitalWrite(DEBUGPIN, LOW);
}


void get_input() {
  if (!Serial.available()) return;
  
  while (Serial.read() != 'A'){ //16
    delay(5);
    if (!Serial.available()) return;
  }
  
  char command = Serial.read();
  connected = true;
  
  switch(command){
    case C_DISCOVER:
      readEmptyPayload();
      Serial.write(C_DISCOVER);
      break;
    case C_HEARTBEAT:
      readEmptyPayload();
      Serial.write(C_HEARTBEAT);
      lastUpdate = millis();
      break;
    case C_LIGHTS:
      for (int i = 0; i < 6; i++){
        while (!Serial.available()){
          delay(1);
        }
        lights[i] = Serial.read();
      }

      lastUpdate = millis();
      break;
    case C_RESET:
      readEmptyPayload();
      resetLights();
      break;
    case C_SPEAKER:
      readEmptyPayload();
      tone(PIEZO, 400, 200);
      break;
    case C_BRIGHTNESS:
      String value = "";
      delay(5);
      value = value + Serial.read();
      delay(5);
      value = value + Serial.read();
      delay(5);
      value = value + Serial.read();
      delay(5);
      value = value + Serial.read();
      brightness = value.toInt();
      Serial.write(brightness);
      break;
  }

}

void updateLights(){
  setLight(RED, lights[0]);
}

void readEmptyPayload(){
  readEmptyPayload(6);
}

void readEmptyPayload(int width){
  for (int i = 0; i < width; i++){
    while (!Serial.available()){
      delay(10);
      Serial.read();
    }
  }
}

void setLight(int light, char status){
  switch(status){
    case 0:
    case '0':
      myservo.write(0);
      analogWrite(light, LOW);
      break;
    case 1:
    case '1':
      myservo.write(90);
      break;
  }
}

void resetLights(){
  connected = false;
  int i;
  for (i = 0; i < 6; i = i + 1) {
    lights[i] = '0';
  }
}

