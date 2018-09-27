CC=@gcc
CFLAGS=-fpic -v
LDFLAGS=-lmpfr -lgmp

OBJS=compute.o crop.o zoom.o test.o

all: clean $(OBJS)
	$(CC) -shared -o mbmpfr/lib/libmbmpfr.so $(addprefix mbmpfr/obj/, $(OBJS))
	$(CC) -o mbmpfr/bin/test $(addprefix mbmpfr/obj/, $(OBJS)) $(LDFLAGS)

debug: CFLAGS += -DDEBUG -g
debug: all

clean:
	@mkdir -p mbmpfr/obj mbmpfr/lib mbmpfr/bin
	@rm -rf mbmpfr/obj/* mbmpfr/lib/* mbmpfr/bin/*

install:
	@cp mbmpfr/lib/libmbmpfr.so /usr/lib/

uninstall:
	@rm -f /usr/lib/libmbmpfr.so

test:
	mbmpfr/bin/./test

%.o : mbmpfr/%.c
	$(CC) -c $(CFLAGS) -o mbmpfr/obj/$@ $<
