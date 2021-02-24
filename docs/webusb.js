class WebUSB {
    constructor(inEndpoint, outEndpoint){
        this.outEndpoint = outEndpoint;
        this.inEndpoint = inEndpoint;
    }
    
    async connect(deviceFilters) {
        this.x=0;
        this.device = await navigator.usb.requestDevice({
            filters:deviceFilters
        });
        
        await this.device.open();
        await this.device.selectConfiguration(1);
        await this.device.claimInterface(0);
    }

    async sendString(message) {
        let encoder = new TextEncoder();
        let bytes = encoder.encode(message); 
        await this.device.transferOut(this.outEndpoint, new Uint8Array([message.length]));
        await this.device.transferOut(this.outEndpoint, bytes);
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
            let result = await this.device.transferIn(this.inEndpoint, count);
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
