export default function reducer(state = {
    nameInput: ""
}, action) {
    switch (action.type) {
        case 'SET_NAME': 
            return {
                ...state,
                nameInput: action.value
            }
        default:
            break;
    }

    return state
}