from langgraph import LangGraph, Node, Edge

def create_workflow():
    graph = LangGraph()

    # Define nodes
    start_node = Node(name="Start", function=lambda ctx: "Welcome to the Stock API!")
    buy_stock = Node(name="Buy Stock", function=lambda ctx: "Stock purchase successful!")
    sell_stock = Node(name="Sell Stock", function=lambda ctx: "Stock sale successful!")
    error_node = Node(name="Error", function=lambda ctx: "Invalid action.")

    # Define edges
    graph.add_edge(Edge(start_node, buy_stock, condition=lambda ctx: ctx.get('action') == 'buy'))
    graph.add_edge(Edge(start_node, sell_stock, condition=lambda ctx: ctx.get('action') == 'sell'))
    graph.add_edge(Edge(start_node, error_node))

    return graph

def execute_workflow(action, user_id):
    # Context for the workflow
    context = {"action": action, "user_id": user_id}
    graph = create_workflow()
    result = graph.run(context)
    return {"response": result}
