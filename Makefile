CC=@gcc
CFLAGS=-Wall -Werror -fpic -v
LDFLAGS=-lmpfr -lgmp

all: clean
	@mkdir -p mbmpfr/obj/ mbmpfr/lib/ 
	$(CC) -c $(CFLAGS) -o mbmpfr/obj/mbmpfr.o mbmpfr/mbmpfr.c
	$(CC) -shared -o mbmpfr/lib/libmbmpfr.so mbmpfr/obj/mbmpfr.o

clean:
	@rm -rf mbmpfr/obj/* mbmpfr/lib/*

install:
	@cp mbmpfr/lib/libmbmpfr.so /usr/lib/

uninstall:
	@rm -f /usr/lib/libmbmpfr.so
