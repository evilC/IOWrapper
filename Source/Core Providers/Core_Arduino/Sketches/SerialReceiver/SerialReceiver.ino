#include <Servo.h>

Servo myservo;  // create servo object to control a servo

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

static float pulseVal = 4.712;

// States (0 off, 1 on, 2 flashing)
char lights[6];
unsigned long lastUpdate;
static boolean connected = false;
int brightness = 10;

void setup() {
  Serial.begin(9600);
  myservo.attach(16);  // attaches the servo on pin 9 to the servo object

  lastUpdate = millis();
  resetLights();
  myservo.write(180);
}

void loop() {
  get_input();

  updateLights();

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
  //setLight(YELLOW, lights[1]);
  //setLight(GREEN, lights[2]);
  //setLight(PEDRED, lights[3]);
  //setLight(PEDGREEN, lights[4]);
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
    case 2:
    case '2':
      pulseLed(light);
      break;
  }
}

void pulseLed(float led){
  float out;
  pulseVal = pulseVal + 0.0002;
  if (pulseVal > 10.995) pulseVal = 4.712;
  out = sin(pulseVal) * 127.5 + 127.5;
  analogWrite(led,out*1.0/6.0);
}

void resetLights(){
  connected = false;
  int i;
  for (i = 0; i < 6; i = i + 1) {
    lights[i] = '0';
  }
}

