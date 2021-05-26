class SerialWebUSB {
    constructor(inEndpoint, outEndpoint){
        this.outEndpoint = outEndpoint;
        this.inEndpoint = inEndpoint;
    }
    
    async connect(deviceFilters) {
        this.x=0;
		
		this.port = await navigator.serial.requestPort({filters:deviceFilters});
		await this.port.open({ baudRate: 9600 });
		this.writer = this.port.writable.getWriter();
		this.reader =  this.port.readable.getReader();
    }

    async sendString(message) {
        let encoder = new TextEncoder();
        let bytes = encoder.encode(message); 
		await this.writer.write(bytes);
    }

    async readString() {
        let messageLengthBuffer = await this.readBytes(1);
        let messageLength = messageLengthBuffer[0];

        let messageBuffer = await this.readBytes(messageLength);
        let decoder = new TextDecoder();
        return decoder.decode(messageBuffer);
    }

    async readBytes(count) {
        let buffer = new Uint8Array(count);
        let offset = 0;
        while (count > 0) {
            let result = await this.reader.read();
            if (result.data) {
                buffer.set(new Uint8Array(result.data.buffer), offset);
                offset += result.data.byteLength; 
                count -= result.data.byteLength;
            }

            if (result.status === 'stall') {
                await this.device.clearHalt(2);
            }
        }

        return buffer;
    }
}
