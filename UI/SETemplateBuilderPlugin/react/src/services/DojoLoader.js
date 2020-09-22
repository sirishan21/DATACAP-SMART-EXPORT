import { MessageService } from "./MessageService.js"

export class DojoLoader {
    constructor(mService) {
        this.createBy = "SETemplateBuilderPlugin"
        if(mService){
            window.icnDojoService = mService;
        }else{
            window.icnDojoService = new MessageService();
            // this.mService = icnDojoService
        }
        this.renderers = {}

        window.icnDojoService.getMessage().subscribe(data => {
            let message = data.payload.message;
            console.log("Message: " + message);
            const renderer = this.renderers[message];
            if (renderer) {
                renderer.call(this, data.payload);
            }
        })
    }

    //Register render function for message hanlder
    registerRender = (message, renderer,context) => {
        if(this.renderers[message]){
            console.log("Message handler is already registered. Please confirm the registration is on purpose.")
        }
        this.renderers[message] = renderer
    }
}
