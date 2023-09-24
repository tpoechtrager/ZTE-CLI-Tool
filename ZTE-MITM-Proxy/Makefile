# Makefile for zte-mitm-proxy

CC = gcc
CFLAGS = -O2 -Wall -fstack-protector-all
LDFLAGS = -lgnutls
TARGET = zte-mitm-proxy
SRC = zte-mitm-proxy.c

all: $(TARGET)

$(TARGET): $(SRC)
	$(CC) $(CFLAGS) $(SRC) $(LDFLAGS) -o $(TARGET)

clean:
	rm -f $(TARGET)
