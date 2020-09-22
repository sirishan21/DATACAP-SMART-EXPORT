import { Subject } from 'rxjs/Subject';
 
export class MessageService {
    constructor() {
        this.createBy = "SETemplateBuilderPlugin"
        this.observable = new Subject();
    }
 
    sendMessage(data) {
        this.observable.next({ payload: data });
    }
 
    clearMessage() {
        this.observable.next();
    }
 
    getMessage() {
        return this.observable;
    }
}
